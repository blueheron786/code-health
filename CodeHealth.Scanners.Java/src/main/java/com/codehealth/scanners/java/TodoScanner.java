package com.codehealth.scanners.java;

import java.util.*;
import java.io.*;
import java.nio.file.*;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

public class TodoScanner {
    public static void scan(String sourceRoot, String outputFile) {
        JSONArray todos = new JSONArray();

        try {
            Files.walk(Paths.get(sourceRoot))
                .filter(Files::isRegularFile)
                .filter(p -> p.toString().endsWith(".java"))
                .forEach(filePath -> {
                    try {
                        List<String> lines = Files.readAllLines(filePath);
                        String relativePath = Paths.get(sourceRoot).relativize(filePath).toString().replace("\\", "/");
                        
                        for (int i = 0; i < lines.size(); i++) {
                            String line = lines.get(i).trim();
                            if (line.matches(".*//\\s*TODO:?.*") || 
                                line.matches(".*/\\*\\s*TODO:?.*") ||
                                line.matches(".*\\*\\s*TODO:?.*")) {
                                JSONObject todo = new JSONObject();
                                todo.put("file", relativePath);
                                todo.put("line", i + 1);
                                todo.put("text", line);
                                todos.add(todo);
                            }
                        }
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                });

            try (FileWriter writer = new FileWriter(outputFile)) {
                writer.write(todos.toJSONString());
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}