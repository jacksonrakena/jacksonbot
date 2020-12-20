package com.abyssaldev.abyss.http.modules

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.Interaction
import io.ktor.application.*
import io.ktor.response.*
import io.ktor.request.*
import io.ktor.routing.*
import io.ktor.http.*
import com.fasterxml.jackson.databind.*
import com.goterl.lazycode.lazysodium.utils.Key
import io.ktor.jackson.*
import io.ktor.features.*
import org.slf4j.LoggerFactory
import org.slf4j.event.*

@JvmOverloads
fun Application.mainModuleWeb(testing: Boolean = false) {
    install(ContentNegotiation) {
        jackson {
            enable(SerializationFeature.INDENT_OUTPUT)
        }
    }

    install(CallLogging) {
        level = Level.INFO
        filter { call -> call.request.path().startsWith("/") }
    }

    routing {
        post("/discord/interactions") {
            val crypt = AbyssApplication.lazySodiumInstance
            val discordPublicKey = AppConfig.instance.discord.interactionsPublicKey
            val signature = call.request.headers["X-Signature-Ed25519"]
            val timestamp = call.request.headers["X-Signature-Timestamp"]
            val body = call.receiveText()
            val interaction = AppConfig.objectMapper.readValue(body, Interaction::class.java)

            val cryptLog = LoggerFactory.getLogger("Discord Interactions")

            try {
                if (!crypt.cryptoSignVerifyDetached(signature, timestamp+body, Key.fromPlainString(discordPublicKey))) {
                    cryptLog.warn("Received invalid request signature. Signature=${signature} Timestamp=${timestamp} Body=${body} PublicKey=${discordPublicKey}")
                    return@post call.respond(HttpStatusCode.Unauthorized, "Invalid request signature")
                } else {
                    cryptLog.info("Received valid request signature.")
                }
            } catch (e: Throwable) {
                return@post call.respond(HttpStatusCode.Unauthorized, "Error occurred while verifying.")
            }

            if (interaction.type == 1) {
                cryptLog.info("Received interaction PING. Responding...")
                call.respond(hashMapOf("type" to 1))
            }
        }
        get("/") {
            call.respondText("HELLO WORLD!", contentType = ContentType.Text.Plain)
        }

        install(StatusPages) {
            exception<Throwable> { cause ->
                println(cause.message)
                call.respond(HttpStatusCode.InternalServerError)
            }
        }
    }
}
