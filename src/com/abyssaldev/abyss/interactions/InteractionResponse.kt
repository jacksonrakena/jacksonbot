package com.abyssaldev.abyss.interactions

import com.abyssaldev.abyss.interactions.abs.JsonHashable
import java.util.*

data class InteractionResponse(val tts: Boolean = false, val content: String = "", val hideUserMessage: Boolean = false): JsonHashable {
    companion object {
        fun message(content: String, hideUserMessage: Boolean = false) = InteractionResponse(false, content, hideUserMessage)
        fun empty() = InteractionResponse()
    }

    override fun createMap(): HashMap<String, Any> =
        hashMapOf(
            "type" to if (hideUserMessage) {
                if (content == "") {
                    2 // Acknowledge message, but hide user's call and do not reply
                } else {
                    3 // Acknowledge message, but hide user's call and respond with content
                }
            } else {
                if (content == "") {
                    5 // Acknowledge message, show user's call and do not reply
                } else {
                    4 // Acknowledge message, show user's call and reply
                }
            },
            "data" to hashMapOf(
                "content" to content,
            "tts" to tts
            )
        )
}