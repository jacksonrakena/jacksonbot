package com.abyssaldev.abyss.framework.gateway

import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.commands.gateway.AdminCommandSet
import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.abyss.util.respondError
import com.abyssaldev.abyss.util.trySendMessage
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import net.dv8tion.jda.api.JDABuilder
import net.dv8tion.jda.api.MessageBuilder
import net.dv8tion.jda.api.Permission
import net.dv8tion.jda.api.events.message.MessageReceivedEvent
import net.dv8tion.jda.api.hooks.ListenerAdapter
import java.util.*

class GatewayController: Loggable, ListenerAdapter() {
    val commands = listOf(
        AdminCommandSet()
    )

    fun applyListeners(jdaBuilder: JDABuilder) {
        jdaBuilder.addEventListeners(this, GatewayReadyListenerAdapter())
    }

    private fun handleOwnerOnlyCommandError(call: GatewayCommandRequest) {
        call.channel.trySendMessage(MessageBuilder().apply { respondError("This command is only available for Abyss' owner.") }.build())
    }

    private fun handleMissingRequiredRole(call: GatewayCommandRequest, roleId: String) {
        call.channel.trySendMessage(MessageBuilder().apply { respondError("To run this command, you need the role `${call.guild!!.getRoleById(roleId)!!.name}`.") }.build())
    }

    private fun handleMissingRequiredUserPermissions(call: GatewayCommandRequest, permissions: EnumSet<Permission>) {
        call.channel.trySendMessage(MessageBuilder().apply { respondError("To run this command, you need these permissions: ${permissions.map { "`${it.name}`" }.joinToString(", ") }")}.build())
    }

    private fun handleMissingRequiredBotPermissions(call: GatewayCommandRequest, permissions: EnumSet<Permission>) {
        call.channel.trySendMessage(MessageBuilder().apply { respondError("To run this command, I need these permissions: ${permissions.map { "`${it.name}`" }.joinToString(", ") }")}.build())
    }

    override fun onMessageReceived(event: MessageReceivedEvent) {
        GlobalScope.launch(Dispatchers.Default) {
            handleMessageReceived(event)
        }
    }

    private suspend fun handleMessageReceived(event: MessageReceivedEvent) {
        var content = event.message.contentRaw
        if (!content.startsWith(AppConfig.instance.discord.gatewayPrefix, true)) return
        content = content.substring(AppConfig.instance.discord.gatewayPrefix.length)

        val contentSplit = content.split(" ")

        val commandToken = contentSplit[0]
        val command = commands.firstOrNull { it.isMatch(commandToken) } ?: return

        val request = GatewayCommandRequest(event.guild, event.textChannel, event.member, event.author, contentSplit.drop(1), event.jda, event.message)

        // Bot owner check
        if (command.isBotOwnerRestricted && (request.user.id != AppConfig.instance.discord.ownerId)) {
            return handleOwnerOnlyCommandError(request)
        }

        if (request.member != null) {
            // Required role check
            if (!command.requiredRole.isNullOrEmpty() && request.member.roles.none { it.id == command.requiredRole }) {
                return handleMissingRequiredRole(request, command.requiredRole!!)
            }

            // Caller permissions
            if (!request.member.hasPermission(command.requiredUserPermissions)) {
                return handleMissingRequiredUserPermissions(request, command.requiredUserPermissions)
            }

            // Bot permissions
            if (!request.botMember!!.hasPermission(command.requiredBotPermissions)) {
                return handleMissingRequiredBotPermissions(request, command.requiredBotPermissions)
            }
        }

        // Finalization
        try {
            val canInvoke = command.canInvoke(request)
            if (!canInvoke.isNullOrEmpty()) {
                request.channel.trySendMessage(MessageBuilder().apply { respondError(canInvoke) }.build())
                return
            }
            val message = command.invoke(request)
            request.channel.trySendMessage(message.build())
        } catch (e: Throwable) {
            logger.error("Error thrown while processing gateway command ${command.name}", e)
            request.channel.trySendMessage( "There was an internal error running that command. Try again later.")
            return
        }
    }
}