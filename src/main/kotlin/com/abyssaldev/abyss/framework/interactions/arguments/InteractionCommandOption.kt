package com.abyssaldev.abyss.framework.interactions.arguments

import com.abyssaldev.abyss.framework.interactions.InteractionBase

interface InteractionCommandOption: InteractionBase {
    val type: InteractionCommandArgumentType
}