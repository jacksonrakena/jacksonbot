package com.abyssaldev.abyss.interactions

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.interactions.abs.InteractionCommand
import com.abyssaldev.abyss.interactions.commands.CatPictureCommand
import com.abyssaldev.abyss.util.Loggable
import io.ktor.client.request.*
import io.ktor.http.*

class InteractionController: Loggable {
    val commands: Array<InteractionCommand> = arrayOf(
        CatPictureCommand()
    )

    suspend fun registerAllCommandsInGuild(guild: String) {
        val httpClient = AbyssApplication.instance.httpClientEngine
        val discordEngine = AbyssApplication.instance.discordEngine
        for (command in this.commands) {
            val data: HashMap<String, String> = httpClient.post {
                method = HttpMethod.Post
                header("Authorization", "Bot ${AppConfig.instance.discord.botToken}")
                contentType(ContentType.Application.Json)
                url("https://discord.com/api/v8/applications/679925967153922055/guilds/${guild}/commands")
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
                logger.error("Failed to register slash command ${command.name}. Raw response: ${AppConfig.objectMapper.writeValueAsString(data)}");
            }
        }
    }

    fun handleInteractionCommandInvoked(data: Interaction): InteractionResponse {
        val command = commands.firstOrNull { it.name == data.data!!.name }
        if (command == null) {
            logger.error("Received a command invocation for command ${data.data!!.name}, but no command is registered.")
            return InteractionResponse(content = "There was an internal error processing your request.")
        }
        logger.info("Invoking command " + command.name)
        return command.invoke(data)
    }
}