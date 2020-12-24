package com.abyssaldev.abyss.util

import org.bouncycastle.asn1.edec.EdECObjectIdentifiers
import org.bouncycastle.asn1.x509.AlgorithmIdentifier
import org.bouncycastle.asn1.x509.SubjectPublicKeyInfo
import org.bouncycastle.jce.provider.BouncyCastleProvider
import org.bouncycastle.util.encoders.Hex
import java.security.KeyFactory
import java.security.Security
import java.security.Signature
import java.security.spec.X509EncodedKeySpec

class Ed25519Engine {
    companion object {
        private val cryptographyProvider = BouncyCastleProvider().apply(Security::addProvider)

        fun validateEd25519Message(publicKey: String, signature: String, timestamp: String, message: String): Boolean =
            Signature
                .getInstance("ed25519", cryptographyProvider)
                .apply {
                    initVerify(
                        KeyFactory
                            .getInstance("ed25519", cryptographyProvider)
                            .generatePublic(
                                X509EncodedKeySpec(
                                    SubjectPublicKeyInfo(
                                        AlgorithmIdentifier(
                                            EdECObjectIdentifiers.id_Ed25519
                                        ),
                                        Hex.decode(publicKey)
                                    ).encoded
                                )
                            )
                    )
                    update(timestamp.toByteArray())
                    update(message.toByteArray())
                }
                .verify(Hex.decode(signature))
    }
}