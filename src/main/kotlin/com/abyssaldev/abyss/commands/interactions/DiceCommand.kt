package com.abyssaldev.abyss.commands.interactions

import com.abyssaldev.abyss.framework.interactions.InteractionCommand
import com.abyssaldev.abyss.framework.interactions.InteractionCommandRequest
import com.abyssaldev.abyss.framework.interactions.InteractionCommandResponse
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgument
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandArgumentType
import com.abyssaldev.abyss.framework.interactions.arguments.InteractionCommandOption
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

    override suspend fun invoke(call: InteractionCommandRequest, rawArgs: List<Any>): InteractionCommandResponse = respond {
        val sides = call.args.named("number_of_sides").integer!!
        val count = call.args.named("number_of_dice").integer!!

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