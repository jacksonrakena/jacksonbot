package com.abyssaldev.abyss.interactions.commands.models.arguments

import com.abyssaldev.abyss.interactions.InteractionRequest
import com.fasterxml.jackson.annotation.JsonIgnoreProperties
import com.fasterxml.jackson.annotation.JsonRawValue
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.Role
import net.dv8tion.jda.api.entities.TextChannel

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionCommandArgumentChoice {
    var name: String = ""
    @JsonRawValue
    var value: String = ""

    var options: Array<InteractionCommandArgumentChoice> = emptyArray()

    fun getAsUser(request: InteractionRequest): Member? {
        if (value.isEmpty() || request.guild == null) return null
        return request.guild!!.getMemberById(value)
    }

    fun getAsChannel(request: InteractionRequest): TextChannel? {
        if (value.isEmpty() || request.guild == null) return null
        return request.guild!!.getTextChannelById(value)
    }

    fun getAsRole(request: InteractionRequest): Role? {
        if (value.isEmpty() || request.guild == null) return null
        return request.guild!!.getRoleById(value)
    }
}