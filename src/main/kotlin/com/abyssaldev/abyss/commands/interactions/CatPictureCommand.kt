package com.abyssaldev.abyss.commands.interactions

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.framework.interactions.InteractionCommand
import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import io.ktor.client.request.*
import kotlinx.coroutines.runBlocking
import net.dv8tion.jda.api.MessageBuilder

class CatPictureCommand : InteractionCommand() {
    private val catApi = "http://aws.random.cat/meow"

    override val name: String = "cat"
    override val description: String = "Finds a picture of a cat."

    override suspend fun invoke(call: InteractionCommandRequest): MessageBuilder {
        return respond(runBlocking {
            AbyssEngine.instance.httpClientEngine.get<HashMap<String, String>>(catApi)["file"].toString()
        })
    }
}