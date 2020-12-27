package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.util.JsonHashable
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