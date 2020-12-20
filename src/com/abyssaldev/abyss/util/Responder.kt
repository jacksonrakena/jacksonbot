package com.abyssaldev.abyss.util

import net.dv8tion.jda.api.requests.RestAction
import net.dv8tion.jda.api.requests.restaction.MessageAction

interface Responder {
    fun respond(vararg actions: RestAction<*>) {
        actions.forEach {
            it.queue()
        }
    }
}