package com.abyssaldev.abyss.interactions.models

import com.abyssaldev.abyss.interactions.commands.models.arguments.InteractionCommandArgumentChoice
import com.fasterxml.jackson.annotation.JsonIgnoreProperties

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionData {
    lateinit var id: String
    lateinit var name: String
    var options: Array<InteractionCommandArgumentChoice>? = null
}