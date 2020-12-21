package com.abyssaldev.abyss.interactions.commands.models.arguments

import com.abyssaldev.abyss.interactions.abs.JsonHashable
import com.fasterxml.jackson.annotation.JsonProperty
import java.util.*

class InteractionCommandArgument(
    override val name: String,
    override val description: String,
    override val type: InteractionCommandArgumentType,
    val choices: InteractionCommandArgumentChoiceSet = emptyArray(),
    @JsonProperty("required")
    val isRequired: Boolean = false
) : InteractionCommandOption, JsonHashable {
    override fun createMap(): HashMap<String, Any> {
        val hashMapInit = hashMapOf<String, Any>(
            "name" to name,
            "description" to description,
            "type" to type.raw,
            "required" to isRequired
        )
        if (choices.any()) {
            hashMapInit["choices"] = choices
        }
        return hashMapInit
    }

}