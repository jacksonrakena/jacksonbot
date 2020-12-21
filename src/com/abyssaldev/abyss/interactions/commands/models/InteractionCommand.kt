package com.abyssaldev.abyss.interactions.commands.models

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.abyssaldev.abyss.interactions.commands.models.arguments.InteractionCommandOption
import net.dv8tion.jda.api.MessageBuilder
import java.util.*

abstract class InteractionCommand: InteractionBase, InteractionExecutable {
    open val options: Array<InteractionCommandOption> = emptyArray()

    override fun invoke(call: InteractionRequest): MessageBuilder
        = respond("There was an error processing your subcommand.")

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