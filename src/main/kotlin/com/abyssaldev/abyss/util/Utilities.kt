package com.abyssaldev.abyss.util

import com.fasterxml.jackson.databind.ObjectMapper
import com.fasterxml.jackson.databind.SerializationFeature
import net.dv8tion.jda.api.MessageBuilder
import net.dv8tion.jda.api.entities.Message
import net.dv8tion.jda.api.entities.MessageChannel
import org.jetbrains.annotations.Contract
import java.awt.Color
import java.io.File
import kotlin.math.round

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

fun <T> ObjectMapper.write(value: T, indentOutput: Boolean = false): String {
    var writer = this.writer()
    if (indentOutput) {
        writer = writer.with(SerializationFeature.INDENT_OUTPUT)
    }
    return writer.writeValueAsString(value)
}

infix fun Double.round(decimals: Int): Double {
    var multiplier = 1.0
    repeat(decimals) { multiplier *= 10 }
    return round(this * multiplier) / multiplier
}

fun MessageBuilder.respondSuccess(content: String) {
    this.setContent(":ballot_box_with_check: ${content}")
}

fun MessageBuilder.respondWarning(content: String) {
    this.setContent(":warning: ${content}")
}

fun MessageBuilder.respondError(content: String) {
    this.setContent(":x: ${content}")
}

inline fun <reified T> List<Annotation>.getAnnotation(): T? {
    return this.filterIsInstance<T>().firstOrNull()
}

inline fun <reified T> List<Annotation>.getAnnotations(): List<T> {
    return this.filterIsInstance<T>().toList()
}