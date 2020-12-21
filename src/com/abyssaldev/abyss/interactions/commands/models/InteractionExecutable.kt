package com.abyssaldev.abyss.interactions.commands.models

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse

interface InteractionExecutable {
    fun invoke(call: InteractionRequest): InteractionResponse

    fun respond(content: String) = InteractionResponse.message(content)
}