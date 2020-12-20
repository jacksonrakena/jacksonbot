package com.abyssaldev.abyss.interactions

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.abs.InteractionCommand
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
        val httpClient = AbyssApplication.instance.httpClientEngine
        try {
            val data: HashMap<String, String> = httpClient.post {
                method = HttpMethod.Post
                header("Authorization", "Bot ${AppConfig.instance.discord.botToken}")
                contentType(ContentType.Application.Json)
                url(if (!command.isGuildLocked) {
                    "https://discord.com/api/v8/applications/${appInfo.id}/commands"
                } else {
                    "https://discord.com/api/v8/applications/${appInfo.id}/guilds/${command.guildLock}/commands"
                })
                body = hashMapOf(
                    "name" to command.name,
                    "description" to command.description,
                    "options" to if (command.arguments != null) {
                        command.arguments!!.map {
                            hashMapOf(
                                "name" to it.name,
                                "description" to it.description,
                                "type" to it.type.raw,
                                "required" to it.isRequired,
                                "choices" to if (it.choices != null) {
                                    it.choices.map { choice ->
                                        hashMapOf(
                                            "name" to choice.name,
                                            "value" to choice.value
                                        )
                                    }
                                } else {
                                    emptyArray<String>()
                                }
                            )
                        }
                    } else {
                        emptyArray<String>()
                    }
                )
            }
            if (data["name"] == command.name) {
                logger.info("Registered slash command ${command.name} to ${if (command.isGuildLocked) { command.guildLock } else { "global scope" }}")
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
        logger.info("Invoking command " + command.name)
        return try {
            command.invoke(
                InteractionRequest(data.guildId, data.channelId, data.member, data.data!!.options ?: emptyArray<InteractionCommandOption>())
            )
        } catch (e: Throwable) {
            logger.error("Error thrown while processing command ${command.name}", e)
            InteractionResponse(content = "There was an internal error running that command. Try again later.")
        }
    }
}