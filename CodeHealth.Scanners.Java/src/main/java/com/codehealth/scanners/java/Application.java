package com.codehealth.scanners.java;

import java.nio.file.Paths;

public class Application {
    public static void main(String[] args) {
        if (args.length != 2) {
            System.out.println("Usage: java -jar CodeHealth.Scanners.Java.jar <sourceRootPath> <outputDirectory>");
            System.exit(1);
        }
        
        String sourceRoot = args[0];
        String outputDir = args[1];
        
        CyclomaticComplexityScanner.scan(sourceRoot, Paths.get(outputDir, "complexity.json").toString());
        TodoScanner.scan(sourceRoot, Paths.get(outputDir, "todos.json").toString());
    }
}