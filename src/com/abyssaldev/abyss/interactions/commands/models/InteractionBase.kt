package com.abyssaldev.abyss.interactions.commands.models

import com.abyssaldev.abyss.interactions.abs.JsonHashable
import com.fasterxml.jackson.annotation.JsonIgnore

interface InteractionBase: JsonHashable {
    val name: String
    val description: String

    @get:JsonIgnore
    val guildLock: Long
        get() = -1

    @get:JsonIgnore
    val isGuildLocked
        get() = guildLock != (-1).toLong()
}