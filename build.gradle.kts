import org.jetbrains.kotlin.gradle.tasks.KotlinCompile

plugins {
    kotlin("jvm") version "1.4.21"
    application
}

group = "com.abyssaldev"
version = "16.1.0"

repositories {
    jcenter()
    mavenCentral()
    mavenLocal()
    maven("https://plugins.gradle.org/m2/")
    maven("https://kotlin.bintray.com/ktor")
    maven(url = "https://kotlin.bintray.com/kotlinx/")
}

val ktorVersion = "1.4.2"
val jdaVersion = "4.2.0_223"
val kotlinVersion = "1.4.21"
val bouncyCastleVersion = "1.67"
val logbackVersion = "1.3.0-alpha5"
val jdaUtilitiesVersion = "3.0.5"

dependencies {
    implementation(kotlin("stdlib"))
    implementation("io.ktor:ktor-server-netty:$ktorVersion")
    implementation("io.ktor:ktor-client-core:$ktorVersion")
    implementation("io.ktor:ktor-client-apache:$ktorVersion")
    implementation("io.ktor:ktor-client-json:$ktorVersion")
    implementation("io.ktor:ktor-client-jackson:$ktorVersion")
    implementation("ch.qos.logback:logback-classic:$logbackVersion")
    implementation("io.ktor:ktor-server-core:$ktorVersion")
    implementation("io.ktor:ktor-jackson:$ktorVersion")
    implementation("io.ktor:ktor-server-host-common:$ktorVersion")
    implementation("net.dv8tion:JDA:$jdaVersion")
    implementation("org.bouncycastle:bcprov-jdk15on:$bouncyCastleVersion")
    implementation("org.apache.commons:commons-lang3:3.11")
    implementation("com.jagrosh:jda-utilities:$jdaUtilitiesVersion")
}

tasks.withType<KotlinCompile> {
    kotlinOptions.jvmTarget = "14"
}

application {
    mainClassName = "com.abyssaldev.abyss.MainKt"
}