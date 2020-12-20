package com.abyssaldev.abyss.gateway

import net.dv8tion.jda.api.requests.RestAction

interface GatewayResponder {
    fun respond(vararg actions: RestAction<*>) {
        actions.forEach {
            it.queue()
        }
    }
}