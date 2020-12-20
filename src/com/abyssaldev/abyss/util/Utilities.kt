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

fun time(func: () -> Unit): Long {
    val start = System.currentTimeMillis()
    func()
    return System.currentTimeMillis() - start
}

fun validateEd25519Message(publicKey: String, signature: String, timestamp: String, message: String): Boolean {
    val provider = BouncyCastleProvider()
    Security.addProvider(provider)
    val pki = SubjectPublicKeyInfo(AlgorithmIdentifier(EdECObjectIdentifiers.id_Ed25519), Hex.decode(publicKey));
    val pkSpec = X509EncodedKeySpec(pki.encoded)
    val kf = KeyFactory.getInstance("ed25519", provider);
    val publicKeySet = kf.generatePublic(pkSpec);
    val signedData = Signature.getInstance("ed25519", provider);
    signedData.initVerify(publicKeySet)
    signedData.update(timestamp.toByteArray())
    signedData.update(message.toByteArray())
    return signedData.verify(Hex.decode(signature));
}