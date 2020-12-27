package com.abyssaldev.abyss.framework.common

import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.entities.Guild
import net.dv8tion.jda.api.entities.Member
import net.dv8tion.jda.api.entities.TextChannel
import net.dv8tion.jda.api.entities.User

abstract class CommandRequest {
    abstract val guild: Guild?
    abstract val channel: TextChannel
    abstract val member: Member?
    abstract val user: User
    abstract val jda: JDA
    internal abstract val rawArgs: HashMap<String, String>
    open val args: ArgumentSet by lazy { ArgumentSet(rawArgs, this) }
}

