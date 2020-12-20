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

    suspend fun registerCommand(appInfo: ApplicationInfo, command: InteractionCommand, guild: String?) {
        val httpClient = AbyssApplication.instance.httpClientEngine
        val data: HashMap<String, String> = httpClient.post {
            method = HttpMethod.Post
            header("Authorization", "Bot ${AppConfig.instance.discord.botToken}")
            contentType(ContentType.Application.Json)
            url(if (guild == null) {
                "https://discord.com/api/v8/applications/${appInfo.id}/commands"
            } else {
                "https://discord.com/api/v8/applications/${appInfo.id}/guilds/${guild}/commands"
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
            logger.info("Registered slash command ${command.name}.")
        } else {
            logger.error("Failed to register slash command ${command.name}. Raw response: ${AppConfig.objectMapper.writeValueAsString(data)}")
        }
    }

    suspend fun registerAllCommandsInGuild(appInfo: ApplicationInfo, guild: String) {
        for (command in this.commands) {
            registerCommand(appInfo, command, guild)
        }
    }

    fun handleInteractionCommandInvoked(data: Interaction): InteractionResponse {
        val command = commands.firstOrNull { it.name == data.data!!.name }
        if (command == null) {
            logger.error("Received a command invocation for command ${data.data!!.name}, but no command is registered.")
            return InteractionResponse(content = "That command has been removed from Abyss.")
        }
        logger.info("Invoking command " + command.name)
        return command.invoke(data)
    }
}