package com.abyssaldev.abyss.framework.gateway

import com.abyssaldev.abyss.framework.common.CommandRequest
import net.dv8tion.jda.api.JDA
import net.dv8tion.jda.api.entities.*

class GatewayCommandRequest(
    override val guild: Guild?,
    override val channel: TextChannel,
    override val member: Member?,
    override val user: User,
    override val rawArgs: List<String>,
    override val jda: JDA,
    val message: Message
): CommandRequest() {
    val botUser: SelfUser by lazy {
        jda.selfUser
    }

    val botMember: Member? by lazy {
        guild?.selfMember
    }
}