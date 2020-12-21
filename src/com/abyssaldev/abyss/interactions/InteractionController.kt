package com.abyssaldev.abyss.interactions

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.commands.models.InteractionCommand
import com.abyssaldev.abyss.interactions.commands.models.InteractionSubcommand
import com.abyssaldev.abyss.interactions.commands.models.InteractionSubcommandGroup
import com.abyssaldev.abyss.interactions.models.Interaction
import com.abyssaldev.abyss.util.Loggable
import io.ktor.client.request.*
import io.ktor.http.*
import net.dv8tion.jda.api.entities.ApplicationInfo

class InteractionController: Loggable {
    private val commands: ArrayList<InteractionCommand> = arrayListOf()

    fun addCommand(command: InteractionCommand) {
        if (commands.any { it.name == command.name }) {
            logger.error("Cannot register two commands with the same name. (Command name=${command.name})")
        }
        commands.add(command)
    }

    fun addCommands(vararg commands: InteractionCommand) {
        for (command in commands) {
            addCommand(command)
        }
    }

    fun getAllCommands() = commands

    suspend fun registerCommand(appInfo: ApplicationInfo, command: InteractionCommand) {
        logger.info(AbyssApplication.objectMapper.writeValueAsString(command.createMap()))
        val httpClient = AbyssApplication.instance.httpClientEngine
        try {
            val data: HashMap<String, Object> = httpClient.post {
                method = HttpMethod.Post
                header("Authorization", "Bot ${AppConfig.instance.discord.botToken}")
                contentType(ContentType.Application.Json)
                url(if (!command.isGuildLocked) {
                    "https://discord.com/api/v8/applications/${appInfo.id}/commands"
                } else {
                    "https://discord.com/api/v8/applications/${appInfo.id}/guilds/${command.guildLock}/commands"
                })
                body = command.createMap()
            }
            if (data["name"].toString() == command.name) {
                logger.info("Registered slash command ${command.name} to ${if (command.isGuildLocked) { command.guildLock } else { "global scope" }}")
                command.options.filterIsInstance<InteractionSubcommand>().forEach {
                    logger.info("Registered slash subcommand ${it.name} (of command ${command.name}) to ${if (command.isGuildLocked) { command.guildLock } else { "global scope" }}")
                }
            } else {
                logger.error("Failed to register slash command ${command.name}. Raw response: ${AbyssApplication.objectMapper.writeValueAsString(data)}")
            }
        } catch (e: Exception) {
            logger.error("Failed to register slash command ${command.name} (exception).", e)
        }
    }

    suspend fun registerAllCommandsGlobally(appInfo: ApplicationInfo) {
        logger.info("Registering all commands.")
        for (command in this.commands) {
            registerCommand(appInfo, command)
        }
    }

    suspend fun registerAllInteractions(appInfo: ApplicationInfo) {
        registerAllCommandsGlobally(appInfo)

        logger.info("All interactions registered.")
    }

    fun handleInteractionCommandInvoked(data: Interaction): InteractionResponse {
        if (data.data == null) {
            return InteractionResponse.empty()
        }
        val command = commands.firstOrNull { it.name == data.data!!.name }
        if (command == null) {
            logger.error("Received a command invocation for command ${data.data!!.name}, but no command is registered.")
            return InteractionResponse(content = "That command has been removed from Abyss.")
        }
        val commandSubcommandsOrSubcommandGroups = command.options.filter { it is InteractionSubcommand || it is InteractionSubcommandGroup }
        if (commandSubcommandsOrSubcommandGroups.any()) {
            data.data!!.options!!.forEach {
                val matchingRootSubcommand = commandSubcommandsOrSubcommandGroups.firstOrNull { q ->
                    q.name == it.name && q is InteractionSubcommand
                } as InteractionSubcommand?
                if (matchingRootSubcommand != null) {
                    logger.info("Invoking matching subcommand ${matchingRootSubcommand.name}")
                    return try {
                        matchingRootSubcommand.invoke(InteractionRequest(data.guildId, data.channelId, data.member, data.data!!.options!![0].options))
                    } catch (e: Throwable) {
                        logger.error("Error thrown while processing subcommand ${matchingRootSubcommand.name}", e)
                        InteractionResponse(content = "There was an internal error running that subcommand. Try again later.")
                    }
                }
            }
        }
        logger.info("Invoking command " + command.name)
        return try {
            command.invoke(
                InteractionRequest(data.guildId, data.channelId, data.member, data.data!!.options ?: emptyArray())
            )
        } catch (e: Throwable) {
            logger.error("Error thrown while processing command ${command.name}", e)
            InteractionResponse(content = "There was an internal error running that command. Try again later.")
        }
    }
}