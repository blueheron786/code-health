package com.codehealth.scanners.java;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;
import java.util.stream.Collectors;

public class Application {
    public static void main(String[] args) {
        if (args.length != 2) {
            System.out.println("Usage: java -jar CodeHealth.Scanners.Java.jar <sourceRootPath> <outputDirectory>");
            System.exit(1);
        }

        String sourceRoot = args[0];
        String outputDir = args[1];

        // Build up a cache of filename => contents in memory, so we don't
        // slam the disk with constant I/O.
        Map<String, String> javaFiles;
        try {
            javaFiles = Files.walk(Paths.get(sourceRoot))
                    .filter(path -> path.toString().endsWith(".java"))
                    .filter(path -> !path.toString().endsWith("Test.java")) // Skip tests
                    .filter(Files::isRegularFile)
                    .collect(Collectors.toMap(
                            Path::toString,
                            path -> {
                                try {
                                    return Files.readString(path, StandardCharsets.UTF_8);
                                } catch (IOException e) {
                                    System.err.println("Failed to read file: " + path);
                                    return "";
                                }
                            }
                    ));
        } catch (IOException e) {
            System.err.println("Failed to walk directory: " + e.getMessage());
            return;
        }

        CyclomaticComplexityScanner.scan(javaFiles, Paths.get(outputDir, "complexity.json").toString());
        TodoScanner.scan(javaFiles, Paths.get(outputDir, "todos.json").toString());
    }
}
