package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.Interaction
import com.abyssaldev.abyss.interactions.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.InteractionResponse
import com.abyssaldev.abyss.interactions.abs.InteractionCommand

class CatPictureCommand : InteractionCommand {
    override val name: String = "cat"
    override val description: String = "Finds a picture of a cat. Registry."
    override fun invoke(invocation: Interaction): InteractionResponse = InteractionResponse(content = "A cat picture would go here.")
}