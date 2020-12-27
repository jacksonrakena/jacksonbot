package com.abyssaldev.abyss.framework.common

import net.dv8tion.jda.api.MessageBuilder

interface CommandExecutable<T: CommandRequest> : CommandBase {
    fun canInvoke(call: T): String? = ""

    suspend fun invoke(call: T): MessageBuilder?
}