package com.abyssaldev.abyss.interactions

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import com.abyssaldev.abyss.interactions.framework.InteractionExecutable
import com.abyssaldev.abyss.interactions.framework.InteractionRequest
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentChoiceSet
import com.abyssaldev.abyss.interactions.framework.subcommands.InteractionSubcommand
import com.abyssaldev.abyss.interactions.framework.subcommands.InteractionSubcommandGroup
import com.abyssaldev.abyss.interactions.models.Interaction
import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.abyss.util.trySendMessage
import com.abyssaldev.abyss.util.write
import io.ktor.client.request.*
import io.ktor.http.*

class InteractionController: Loggable {
    private val commands: ArrayList<InteractionCommand> = arrayListOf()

    fun addCommand(command: InteractionCommand) {
        if (commands.any { it.name == command.name }) {
            return logger.error("Cannot register two commands with the same name. (Command name=${command.name})")
        }
        commands.add(command)
    }

    fun addCommands(vararg commands: InteractionCommand) {
        for (command in commands) {
            addCommand(command)
        }
    }

    fun getAllCommands() = commands

    private fun getGuildIdRegistrant(command: InteractionCommand): String {
        if (command.isGuildLocked) return command.guildLock.toString()
        val scope = AppConfig.instance.determineCommandScope(command.name)
        return if (!scope.isNullOrEmpty()) scope
        else ""
    }

    private suspend fun registerCommand(command: InteractionCommand) {
        val httpClient = AbyssEngine.instance.httpClientEngine
        val id = getGuildIdRegistrant(command)
        val applicationId = AbyssEngine.instance.discordEngine.selfUser.id
        try {
            val data: HashMap<String, Any> = httpClient.post {
                method = HttpMethod.Post
                header("Authorization", "Bot ${AppConfig.instance.discord.botToken}")
                contentType(ContentType.Application.Json)
                url(if (id == "") {
                    "https://discord.com/api/v8/applications/${applicationId}/commands"
                } else {
                    "https://discord.com/api/v8/applications/${applicationId}/guilds/${id}/commands"
                })
                body = command.toJsonMap()
            }
            if (data["name"].toString() == command.name) {
                logger.info("Registered interaction ${command::class.simpleName}(name = ${command.name}) to ${if (id != "") { id } else { "global scope" }}")
                command.options.filterIsInstance<InteractionSubcommand>().forEach {
                    logger.info("Registered interaction subcommand ${it.name} (of command ${command::class.simpleName}) to ${if (id != "") { id } else { "global scope" }}")
                }
            } else {
                logger.error("Failed to register slash command ${command.name}. Raw response: ${AbyssEngine.jsonEngine.write(data)}")
            }
        } catch (e: Exception) {
            logger.error("Failed to register slash command ${command.name} (exception).", e)
        }
    }

    suspend fun registerAllInteractions() {
        logger.info("Registering all interactions for ${AbyssEngine.instance.discordEngine.selfUser.id}.")
        for (command in this.commands) {
            registerCommand(command)
        }

        logger.info("All interactions registered.")
    }

    fun handleInteractionCommandInvoked(raw: Interaction) {
        val data = raw.data ?: return
        val channelId = raw.channelId ?: return
        val command = commands.firstOrNull { it.name == data.name }
        if (command == null) {
            logger.error("Received a command invocation for command ${data.name}, but no command is registered.")
            AbyssEngine.instance.discordEngine.getTextChannelById(channelId)?.trySendMessage("That command has been disabled.")
            return
        }
        val commandSubcommandsOrSubcommandGroups = command.options.filter { it is InteractionSubcommand || it is InteractionSubcommandGroup }

        var executable: InteractionExecutable = command
        var arguments: InteractionCommandArgumentChoiceSet = data.options ?: emptyArray()
        if (commandSubcommandsOrSubcommandGroups.any()) {
            data.options!!.forEach {
                val matchingRootSubcommand = commandSubcommandsOrSubcommandGroups.firstOrNull { q ->
                    q.name == it.name && q is InteractionSubcommand
                } as InteractionSubcommand?

                if (matchingRootSubcommand != null) {
                    logger.info("Matched subcommand ${matchingRootSubcommand.name}")
                    executable = matchingRootSubcommand
                    arguments = data.options!![0].options
                }
            }
        }

        try {
            val interactionRequest = InteractionRequest(raw.guildId, raw.channelId, raw.member!!, arguments)
            val canInvoke = executable.canInvoke(interactionRequest)
            if (canInvoke != null) {
                AbyssEngine.instance.discordEngine.getTextChannelById(channelId)?.trySendMessage(canInvoke)
                return
            }
            val message = executable.invoke(interactionRequest)
            if (message != null) {
                AbyssEngine.instance.discordEngine.getTextChannelById(channelId)?.trySendMessage(message.build())
            }
        } catch (e: Throwable) {
            logger.error("Error thrown while processing ${if (executable is InteractionSubcommand) { "sub" } else {""}}command ${command.name}", e)
            AbyssEngine.instance.discordEngine.getTextChannelById(channelId)?.trySendMessage( "There was an internal error running that command. Try again later.")
            return
        }
    }
}