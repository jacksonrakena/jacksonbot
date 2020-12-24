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
import net.dv8tion.jda.api.entities.ApplicationInfo

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

    private fun getGuildIdRegistrant(appInfo: ApplicationInfo, command: InteractionCommand): String {
        if (command.isGuildLocked) return command.guildLock.toString() //"https://discord.com/api/v8/applications/${appInfo.id}/guilds/${command.guildLock}/commands"
        val scope = AppConfig.instance.determineCommandScope(command.name)
        return if (!scope.isNullOrEmpty()) scope //"https://discord.com/api/v8/applications/${appInfo.id}/guilds/${scope}/commands"
        else "" //""https://discord.com/api/v8/applications/${appInfo.id}/commands"
    }

    private suspend fun registerCommand(appInfo: ApplicationInfo, command: InteractionCommand) {
        val httpClient = AbyssEngine.instance.httpClientEngine
        val id = getGuildIdRegistrant(appInfo, command)
        try {
            val data: HashMap<String, Any> = httpClient.post {
                method = HttpMethod.Post
                header("Authorization", "Bot ${AppConfig.instance.discord.botToken}")
                contentType(ContentType.Application.Json)
                url(if (id == "") {
                    "https://discord.com/api/v8/applications/${appInfo.id}/commands"
                } else {
                    "https://discord.com/api/v8/applications/${appInfo.id}/guilds/${id}/commands"
                })
                body = command.toJsonMap()
            }
            if (data["name"].toString() == command.name) {
                logger.info("Registered slash command ${command.name} to ${if (id != "") { id } else { "global scope" }}")
                command.options.filterIsInstance<InteractionSubcommand>().forEach {
                    logger.info("Registered slash subcommand ${it.name} (of command ${command.name}) to ${if (id != "") { id } else { "global scope" }}")
                }
            } else {
                logger.error("Failed to register slash command ${command.name}. Raw response: ${AbyssEngine.jsonEngine.write(data)}")
            }
        } catch (e: Exception) {
            logger.error("Failed to register slash command ${command.name} (exception).", e)
        }
    }

    suspend fun registerAllInteractions(appInfo: ApplicationInfo) {
        logger.info("Registering all commands.")
        for (command in this.commands) {
            registerCommand(appInfo, command)
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