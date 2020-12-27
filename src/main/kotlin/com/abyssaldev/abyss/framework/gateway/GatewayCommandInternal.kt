package com.abyssaldev.abyss.framework.gateway

import com.abyssaldev.abyss.framework.common.CommandBase
import com.abyssaldev.abyss.framework.common.CommandExecutable
import com.abyssaldev.abyss.framework.common.CommandModule
import net.dv8tion.jda.api.MessageBuilder
import net.dv8tion.jda.api.Permission
import java.util.*
import kotlin.reflect.KFunction

class GatewayCommandInternal(
    override val name: String,
    override val description: String,
    val isBotOwnerRestricted: Boolean,
    val requiredRole: String,
    val requiredUserPermissions: EnumSet<Permission>,
    val requiredBotPermissions: EnumSet<Permission>,
    val invoke: KFunction<*>,
    val parentModule: CommandModule) : CommandBase, CommandExecutable<GatewayCommandRequest> {

    internal fun isMatch(token: String): Boolean {
        return this.name == token
    }

    override suspend fun invoke(call: GatewayCommandRequest): MessageBuilder? {
        return invoke.call(parentModule, call) as? MessageBuilder
    }
}
