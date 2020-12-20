package com.abyssaldev.abyss.util

fun time(func: () -> Unit): Long {
    val start = System.currentTimeMillis()
    func()
    return System.currentTimeMillis() - start
}