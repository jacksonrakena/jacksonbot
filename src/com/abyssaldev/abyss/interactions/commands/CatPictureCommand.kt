package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.AbyssApplication
import com.abyssaldev.abyss.interactions.Interaction
import com.abyssaldev.abyss.interactions.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.abs.InteractionCommand
import io.ktor.client.request.*
import kotlinx.coroutines.runBlocking

class CatPictureCommand : InteractionCommand {
    private val catApi = "http://aws.random.cat/meow"

    override val name: String = "cat"
    override val description: String = "Finds a picture of a cat."
    override fun invoke(invocation: Interaction): InteractionResponse {
        return InteractionResponse(content = runBlocking {
            val response: HashMap<String, String> = AbyssApplication.instance.httpClientEngine.get(catApi)
            return@runBlocking response["file"]
        }!!)
    }
}