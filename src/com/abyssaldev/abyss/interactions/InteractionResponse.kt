package com.abyssaldev.abyss.interactions

data class InteractionResponse(val tts: Boolean = false, val content: String = "", val hideUserMessage: Boolean = false) {
    companion object {
        fun message(content: String, hideUserMessage: Boolean ) = InteractionResponse(false, content, hideUserMessage)
        fun empty() = InteractionResponse()
    }
}