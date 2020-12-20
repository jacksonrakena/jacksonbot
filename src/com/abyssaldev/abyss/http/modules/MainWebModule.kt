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
import io.ktor.jackson.*
import io.ktor.features.*
import org.bouncycastle.asn1.edec.EdECObjectIdentifiers
import org.bouncycastle.asn1.x509.AlgorithmIdentifier
import org.bouncycastle.asn1.x509.SubjectPublicKeyInfo
import org.bouncycastle.jce.provider.BouncyCastleProvider
import org.bouncycastle.util.encoders.Hex
import org.slf4j.LoggerFactory
import org.slf4j.event.*
import java.security.KeyFactory
import java.security.Security
import java.security.Signature
import java.security.spec.X509EncodedKeySpec

fun validate(pubkey: String, signature: String, timestamp: String, message: String): Boolean {
    val provider = BouncyCastleProvider()
    Security.addProvider(provider)
    val pki = SubjectPublicKeyInfo(AlgorithmIdentifier(EdECObjectIdentifiers.id_Ed25519), Hex.decode(pubkey));
    val pkSpec = X509EncodedKeySpec(pki.encoded)
    val kf = KeyFactory.getInstance("ed25519", provider);
    val publicKey = kf.generatePublic(pkSpec);
    val signedData = Signature.getInstance("ed25519", provider);
    signedData.initVerify(publicKey)
    signedData.update(timestamp.toByteArray())
    signedData.update(message.toByteArray())
    return signedData.verify(Hex.decode(signature));
}

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
            val discordPublicKey = AppConfig.instance.discord.interactionsPublicKey
            val signature = call.request.headers["X-Signature-Ed25519"]
            val timestamp = call.request.headers["X-Signature-Timestamp"]
            val body = call.receiveText()
            val interaction = AppConfig.objectMapper.readValue(body, Interaction::class.java)

            val cryptLog = LoggerFactory.getLogger("Discord Interactions")
            try {
                if (validate(discordPublicKey, signature!!, timestamp!!, body)) {
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
