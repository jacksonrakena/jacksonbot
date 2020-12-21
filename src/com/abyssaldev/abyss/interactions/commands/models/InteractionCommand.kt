package com.abyssaldev.abyss.interactions.commands.models

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.InteractionResponse
import java.util.HashMap

abstract class InteractionCommand: InteractionBase {
    open val options: Array<InteractionCommandOption> = emptyArray()

    open fun invoke(call: InteractionRequest): InteractionResponse
        = InteractionResponse.message("There was an error processing this command.")

    fun respond(content: String) = InteractionResponse.message(content)

    override fun createMap(): HashMap<String, Any> {
        val hashMapInit = hashMapOf<String, Any>(
            "name" to name,
            "description" to description
        )
        if (options.any()) {
            hashMapInit["options"] = options.map {
                it.createMap()
            }
        }
        return hashMapInit
    }
}