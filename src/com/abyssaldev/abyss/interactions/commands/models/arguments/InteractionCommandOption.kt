package com.abyssaldev.abyss.interactions.commands.models.arguments

import com.abyssaldev.abyss.interactions.commands.models.InteractionBase

interface InteractionCommandOption: InteractionBase {
    val type: InteractionCommandArgumentType
}