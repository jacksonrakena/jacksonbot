package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.abyss.util.JsonHashable
import com.abyssaldev.abyssal_command_engine.framework.common.CommandBase
import com.fasterxml.jackson.annotation.JsonIgnore

interface InteractionBase: CommandBase, JsonHashable {
    @get:JsonIgnore
    val guildLock: Long
        get() = -1

    @get:JsonIgnore
    val isGuildLocked
        get() = guildLock != (-1).toLong()
}