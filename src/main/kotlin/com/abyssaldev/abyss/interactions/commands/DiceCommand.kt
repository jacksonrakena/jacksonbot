package com.abyssaldev.abyss.interactions.commands

import com.abyssaldev.abyss.interactions.framework.InteractionCommand
import com.abyssaldev.abyss.interactions.framework.InteractionRequest
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.interactions.framework.arguments.InteractionCommandOption
import net.dv8tion.jda.api.MessageBuilder
import kotlin.random.Random

class DiceCommand : InteractionCommand() {
    override val name = "roll"
    override val description = "Rolls a specified number of dice."

    override val options: Array<InteractionCommandOption> = arrayOf(
        InteractionCommandArgument(
            name = "number_of_sides",
            description = "How many sides does the dice have?",
            type = InteractionCommandArgumentType.Integer,
            isRequired = true
        ),
        InteractionCommandArgument(
            name = "number_of_dice",
            description = "How many dice are we rolling?",
            type = InteractionCommandArgumentType.Integer,
            isRequired = true
        )
    )

    override suspend fun invoke(call: InteractionRequest): MessageBuilder = respond {
        val sides = call.arguments[0].value.toInt()
        val count = call.arguments[1].value.toInt()

        if (sides <= 1 || count > 30 || count < 1 || sides > 30) {
            appendLine("I can only roll 1-30 dice, each with 2-30 sides.")
            return@respond
        }

        if (count == 1) content("I rolled ${roll(sides)}.")
        else {
            for (die0 in 1..count) {
                appendLine("**Die ${die0}**: ${roll(sides)}")
            }
        }
    }

    private fun roll(sides: Int): Int {
        return Random.nextInt(until = sides)+1
    }
}