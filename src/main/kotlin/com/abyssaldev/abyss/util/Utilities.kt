package com.abyssaldev.abyss.util

import com.fasterxml.jackson.databind.ObjectMapper
import net.dv8tion.jda.api.entities.Message
import net.dv8tion.jda.api.entities.MessageChannel
import org.jetbrains.annotations.Contract
import java.awt.Color
import java.io.File

inline fun <T> tryAndIgnoreExceptions(f: () -> T) =
    try {
        f()
    } catch (_: Exception) {

    }

fun MessageChannel.trySendMessage(m: Message) = tryAndIgnoreExceptions { this.sendMessage(m).queue() }
fun MessageChannel.trySendMessage(m: CharSequence) = tryAndIgnoreExceptions { this.sendMessage(m).queue() }

@Contract(pure = true)
fun String.parseHex(): Color? {
    val hex = this.replace("#", "")

    if (hex.length < 6) return null

    return Color(
        Integer.valueOf(hex.substring(0, 2), 16),
        Integer.valueOf(hex.substring(2, 4), 16),
        Integer.valueOf(hex.substring(4, 6), 16),
        if (hex.length == 8) {
            Integer.valueOf(hex.substring(6, 8), 16)
        } else {
            255
        }
    )
}

inline fun <reified T> ObjectMapper.read(value: String): T {
    return readValue(value, T::class.java)
}

inline fun <reified T> ObjectMapper.read(value: File): T {
    return readValue(value, T::class.java)
}

fun <T> ObjectMapper.write(value: T): String {
    return writeValueAsString(value)
}
