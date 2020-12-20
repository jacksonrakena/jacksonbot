package com.abyssaldev.abyss.interactions

enum class InteractionResponseType(val raw: Int) {
    Ignore(2),
    Message(3)
}

data class InteractionResponse(val tts: Boolean = false, val content: String = "", val hideUserMessage: Boolean = false)