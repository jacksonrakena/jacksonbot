package com.abyssaldev.abyss.framework.gateway

import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.entities.*

class GatewayCommand {
}

interface GatewayExecutable {
    fun canInvoke(call: GatewayCommandCall): Boolean
    suspend fun invoke(call: GatewayCommandCall)
}

class GatewayCommandCall(
    val jda: JDA,
    val channel: TextChannel,
    val guild: Guild?,
    val user: User,
    val member: Member?,
    val message: Message
) {
    val content
        get() = message.contentRaw
}

