package com.abyssaldev.abyss.interactions.http

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.InteractionController
import com.abyssaldev.abyss.interactions.models.Interaction
import com.abyssaldev.abyss.util.Ed25519Engine
import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.abyss.util.read
import io.ktor.application.*
import io.ktor.features.*
import io.ktor.http.*
import io.ktor.request.*
import io.ktor.response.*
import io.ktor.routing.*
import kotlinx.coroutines.*
import org.slf4j.LoggerFactory

class DiscordInteractionRouting : Loggable {
    companion object {
        private val pingAcknowledgeHashMap = hashMapOf("type" to 1)
        private val emptyMessageAcknowledgeHashMap = hashMapOf("type" to 5)
        private val internalDecoderErrorAcknowledgeHashMap = hashMapOf(
            "type" to 4,
            "data" to hashMapOf(
                "content" to "Hey there! There appears to be an internal issue affecting Abyss commands. Please try again later.",
                "tts" to false
            )
        )

        fun Application.discordInteractionRouting() {
            routing {
                // Handle all Discord interaction requests
                post(AppConfig.instance.web.interactionsRoute) {
                    // Read Discord's encryption information and verify integrity
                    val discordPublicKey = AppConfig.instance.discord.interactionsPublicKey
                    val signature = call.request.headers["X-Signature-Ed25519"] ?: return@post call.respond(
                        HttpStatusCode.BadRequest, "Missing signature."
                    )
                    val timestamp = call.request.headers["X-Signature-Timestamp"] ?: return@post call.respond(
                        HttpStatusCode.BadRequest, "Missing timestamp."
                    )
                    val stringContent = try {
                        call.receiveText()
                    } catch (e: Exception) {
                        return@post call.respond(
                            HttpStatusCode.BadRequest, "Invalid content."
                        )
                    }
                    val interactionLogger = LoggerFactory.getLogger(InteractionController::class.java)

                    try {
                        if (!Ed25519Engine.verifyMessage(discordPublicKey, signature, timestamp, stringContent)) {
                            interactionLogger.warn("Received invalid request signature. Signature=${signature} Timestamp=${timestamp} Body=${stringContent}")
                            return@post call.respond(HttpStatusCode.Unauthorized, "Invalid request signature.")
                        }
                    } catch (e: Throwable) {
                        return@post call.respond(HttpStatusCode.Unauthorized, "An error occurred while validating the request signature.")
                    }

                    // Read JSON data on another coroutine, because it could be quite big
                    val interaction: Interaction = try {
                        withContext(Dispatchers.Default) {
                            AbyssEngine.jsonEngine.read<Interaction>(
                                stringContent
                            )
                        }
                    } catch (e: Exception) {
                        /**
                         * If there was an issue decoding interaction data, it might be due to our models
                         * being out of date or incorrect. So, we'll try to read the data with no strongly
                         * typed models, and if it's a command call, tell the user that we're having issues
                         * right now. Otherwise we'll just reject it with an Internal Server Error and light
                         * up our console.
                         */
                        interactionLogger.error("Critical mapping exception decoding interaction data", e)
                        try {
                            val interactionSimple = AbyssEngine.jsonEngine.read<HashMap<String, Any>>(stringContent)
                            val interactionType = interactionSimple["type"]

                            if (interactionType?.toString() == "2")
                                return@post call.respond(internalDecoderErrorAcknowledgeHashMap)

                            interactionLogger.error(
                                "Failed to decode interaction data, but interaction wasn't a command invocation.",
                                e
                            )
                            return@post call.respond(HttpStatusCode.InternalServerError)
                        } catch (e: Exception) {
                            /**
                             * To get here there must have to have been a MAJOR error either reading or decoding the
                             * data.
                             */
                            interactionLogger.error(
                                "Catastrophic error decoding interaction data - couldn't decode base data.",
                                e
                            )
                            return@post call.respond(HttpStatusCode.InternalServerError)
                        }
                    }

                    when (interaction.type) {
                        // Handle and acknowledge Discord's pings
                        1 -> call.respond(pingAcknowledgeHashMap)

                        // Handle a command call
                        2 -> {
                            // With REST interactions you can reply with the message content,
                            // but it's simpler (and easier) to just use JDA to send a regular message to
                            // the channel ID (which is done with handleInteractionCommandInvoked)
                            call.respond(emptyMessageAcknowledgeHashMap)

                            GlobalScope.launch(Dispatchers.Default) {
                                /**
                                 * Intentional delay, otherwise the Discord user invoked message might show up
                                 * after we send the command response. Discord's fault.
                                 */
                                delay(250)
                                AbyssEngine.instance.interactions.handleInteractionCommandInvoked(interaction)
                            }
                        }

                        /**
                         * There are no other interaction operation codes documented right now.
                         */
                        else -> {
                            return@post call.respond(HttpStatusCode.BadRequest)
                        }
                    }
                }

                install(StatusPages) {
                    // If a route throws, return 500 Internal Server Error
                    exception<Throwable> { cause ->
                        println(cause.message)
                        call.respond(HttpStatusCode.InternalServerError)
                    }
                }
            }
        }

    }
}