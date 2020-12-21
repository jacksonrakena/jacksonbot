package com.abyssaldev.abyss.interactions.framework.arguments

import com.abyssaldev.abyss.interactions.framework.InteractionBase

interface InteractionCommandOption: InteractionBase {
    val type: InteractionCommandArgumentType
}