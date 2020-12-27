package com.abyssaldev.abyss.framework.gateway

import com.abyssaldev.abyss.framework.common.CommandBase
import com.abyssaldev.abyss.framework.common.CommandExecutable
import net.dv8tion.jda.api.Permission
import java.util.*

abstract class GatewayCommand: CommandBase, CommandExecutable<GatewayCommandRequest> {
    companion object {
        internal val emptyPermissionsEnumSet = EnumSet.noneOf(Permission::class.java)!!
    }

    /**
     * Restricts this command to only be used by the bot owner.
     */
    var isBotOwnerRestricted: Boolean = false

    /**
     * Restricts this command to be used by a specific role ID.
     *
     * The bot owner overrides this requirement.
     */
    var requiredRole: String? = null

    /**
     * Requires that the command caller has these permissions.
     *
     * The bot owner overrides these requirements.
     */
    var requiredUserPermissions: EnumSet<Permission> = emptyPermissionsEnumSet

    /**
     * Requires that the bot has these permissions.
     */
    var requiredBotPermissions: EnumSet<Permission> = emptyPermissionsEnumSet

    internal fun isMatch(token: String): Boolean {
        return this.name == token
    }
}
