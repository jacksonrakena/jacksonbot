package com.abyssaldev.abyss.http.modules

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.Interaction
import com.abyssaldev.abyss.interactions.InteractionController
import com.abyssaldev.abyss.util.validateEd25519Message
import com.fasterxml.jackson.databind.*
import com.fasterxml.jackson.module.kotlin.readValue
import io.ktor.application.*
import io.ktor.features.*
import io.ktor.http.*
import io.ktor.jackson.*
import io.ktor.request.*
import io.ktor.response.*
import io.ktor.routing.*
import org.slf4j.LoggerFactory
import org.slf4j.event.*

val pingAcknowledgeHashMap = hashMapOf("type" to 1)

fun Application.discordInteractionModule() {
    routing {
        post("/discord/interactions") {
            val discordPublicKey = AppConfig.instance.discord.interactionsPublicKey
            val signature = call.request.headers["X-Signature-Ed25519"] ?: return@post call.respond(HttpStatusCode.BadRequest, "Missing signature.")
            val timestamp = call.request.headers["X-Signature-Timestamp"] ?: return@post call.respond(HttpStatusCode.BadRequest, "Missing timestamp.")
            val stringContent = try { call.receiveText() } catch (e: Exception) { return@post call.respond(HttpStatusCode.BadRequest, "Invalid content.")}
            val interactionLogger = LoggerFactory.getLogger(InteractionController::class.java)

            try {
                if (!validateEd25519Message(discordPublicKey, signature, timestamp, stringContent)) {
                    interactionLogger.warn("Received invalid request signature. Signature=${signature} Timestamp=${timestamp} Body=${stringContent} PublicKey=${discordPublicKey}")
                    return@post call.respond(HttpStatusCode.Unauthorized, "Invalid request signature.")
                }
            } catch (e: Throwable) {
                return@post call.respond(HttpStatusCode.Unauthorized, "Invalid request signature.")
            }

            val interaction: Interaction = try {
                AppConfig.objectMapper.readValue(stringContent, Interaction::class.java)
            } catch (e: Exception) {
                interactionLogger.error("Mapping exception decoding interaction data", e)
                try {
                    val interactionSimple: HashMap<String, String> = AppConfig.objectMapper.readValue(stringContent)
                    val interactionType = interactionSimple["type"]
                    if (interactionType != null && interactionType == "2") {
                        return@post call.respond(hashMapOf(
                            "type" to 4,
                            "data" to hashMapOf(
                                "content" to "Hey there! There appears to be an internal issue affecting Abyss commands. Please try again later.",
                                "tts" to false
                            )
                        ))
                    }
                    interactionLogger.error("Failed to decode interaction data, but interaction wasn't a command invocation.", e)
                    return@post call.respond(HttpStatusCode.InternalServerError)
                } catch (e: Exception) {
                    interactionLogger.error("Catastrophic error decoding interaction data - couldn't decode base data.", e)
                    return@post call.respond(HttpStatusCode.InternalServerError)
                }
            }

            when (interaction.type) {
                1 -> call.respond(pingAcknowledgeHashMap) // Ping acknowledge
                2 -> { // Application command
                    val response = AbyssApplication.instance.interactions.handleInteractionCommandInvoked(interaction)
                    val typeInt = if (response.hideUserMessage) {
                        if (response.content == "") {
                            2
                        } else {
                            3
                        }
                    } else {
                        if (response.content == "") {
                            5
                        } else {
                            4
                        }
                    }
                    return@post call.respond(hashMapOf(
                        "type" to typeInt,
                        "data" to hashMapOf(
                            "content" to response.content,
                            "tts" to response.tts
                        )
                    ))
                }
            }
        }

        install(StatusPages) {
            exception<Throwable> { cause ->
                println(cause.message)
                call.respond(HttpStatusCode.InternalServerError)
            }
        }
    }
}
