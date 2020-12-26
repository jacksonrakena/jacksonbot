package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import com.abyssaldev.abyss.interactions.framework.InteractionRequest
import io.ktor.client.request.*
import kotlinx.coroutines.runBlocking
import net.dv8tion.jda.api.MessageBuilder

class CatPictureCommand : InteractionCommand() {
    private val catApi = "http://aws.random.cat/meow"

    override val name: String = "cat"
    override val description: String = "Finds a picture of a cat."

    override suspend fun invoke(call: InteractionRequest): MessageBuilder {
        return respond(runBlocking {
            AbyssEngine.instance.httpClientEngine.get<HashMap<String, String>>(catApi)["file"].toString()
        })
    }
}