package com.abyssaldev.abyss.commands.interactions

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.framework.interactions.InteractionCommand
import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import com.abyssaldev.abyss.framework.interactions.InteractionCommandResponse
import io.ktor.client.request.*
import kotlinx.coroutines.runBlocking

class CatPictureCommand : InteractionCommand() {
    private val catApi = "http://aws.random.cat/meow"

    override val name: String = "cat"
    override val description: String = "Finds a picture of a cat."

    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): InteractionCommandResponse {
        return respond(runBlocking {
            AbyssEngine.instance.httpClientEngine.get<HashMap<String, String>>(catApi)["file"].toString()
        })
    }
}