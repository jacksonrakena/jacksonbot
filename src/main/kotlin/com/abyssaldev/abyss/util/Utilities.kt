package com.abyssaldev.abyss.util

import net.dv8tion.jda.api.entities.Message
import net.dv8tion.jda.api.entities.MessageChannel
import org.bouncycastle.asn1.edec.EdECObjectIdentifiers
import org.bouncycastle.asn1.x509.AlgorithmIdentifier
import org.bouncycastle.asn1.x509.SubjectPublicKeyInfo
import org.bouncycastle.jce.provider.BouncyCastleProvider
import org.bouncycastle.util.encoders.Hex
import java.awt.Color
import java.security.KeyFactory
import java.security.Security
import java.security.Signature
import java.security.spec.X509EncodedKeySpec

fun validateEd25519Message(publicKey: String, signature: String, timestamp: String, message: String): Boolean {
    val provider = BouncyCastleProvider()
    Security.addProvider(provider)
    val pki = SubjectPublicKeyInfo(AlgorithmIdentifier(EdECObjectIdentifiers.id_Ed25519), Hex.decode(publicKey))
    val pkSpec = X509EncodedKeySpec(pki.encoded)
    val kf = KeyFactory.getInstance("ed25519", provider)
    val publicKeySet = kf.generatePublic(pkSpec)
    val signedData = Signature.getInstance("ed25519", provider)
    signedData.initVerify(publicKeySet)
    signedData.update(timestamp.toByteArray())
    signedData.update(message.toByteArray())
    return signedData.verify(Hex.decode(signature))
}

inline fun <T> tryAndIgnoreExceptions(f: () -> T) =
    try {
        f()
    } catch (_: Exception) {

    }

fun MessageChannel.trySendMessage(m: Message) = tryAndIgnoreExceptions { this.sendMessage(m).queue() }
fun MessageChannel.trySendMessage(m: CharSequence) = tryAndIgnoreExceptions { this.sendMessage(m).queue() }

fun parseHexString(hex0: String): Color? {
    var hex = hex0
    hex = hex.replace("#", "")
    return when (hex.length) {
        6 -> Color(
            Integer.valueOf(hex.substring(0, 2), 16),
            Integer.valueOf(hex.substring(2, 4), 16),
            Integer.valueOf(hex.substring(4, 6), 16)
        )
        8 -> Color(
            Integer.valueOf(hex.substring(0, 2), 16),
            Integer.valueOf(hex.substring(2, 4), 16),
            Integer.valueOf(hex.substring(4, 6), 16),
            Integer.valueOf(hex.substring(6, 8), 16)
        )
        else -> null
    }
}