package com.codehealth.scanners.java;

import java.util.*;
import java.io.*;
import java.nio.file.*;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

public class CyclomaticComplexityScanner {
    public static void scan(String sourceRoot, String outputFile) {
        JSONObject report = new JSONObject();
        JSONArray files = new JSONArray();
        int totalComplexity = 0;
        int totalMethods = 0;

        try {
            Files.walk(Paths.get(sourceRoot))
                .filter(Files::isRegularFile)
                .filter(p -> p.toString().endsWith(".java"))
                .forEach(filePath -> {
                    try {
                        JSONObject fileJson = new JSONObject();
                        JSONArray methodsArray = new JSONArray();
                        String relativePath = Paths.get(sourceRoot).relativize(filePath).toString().replace("\\", "/");
                        fileJson.put("file", relativePath);

                        List<String> lines = Files.readAllLines(filePath);
                        boolean inMethod = false;
                        int complexity = 1;
                        String methodName = null;

                        for (String line : lines) {
                            line = line.trim();
                            if (line.matches(".*\\b(public|private|protected)?\\s+\\w+\\s+\\w+\\(.*\\)\\s*\\{.*")) {
                                inMethod = true;
                                methodName = line.split("\\(")[0].replaceAll(".*\\s", "");
                                complexity = 1;
                                continue;
                            }
                            
                            if (inMethod) {
                                if (line.contains("{")) complexity++;
                                if (line.contains("if") || line.contains("for") || line.contains("while") ||
                                    line.contains("case") || line.contains("catch") ||
                                    line.contains("&&") || line.contains("||") || line.contains("?")) {
                                    complexity++;
                                }

                                if (line.contains("}")) {
                                    JSONObject methodJson = new JSONObject();
                                    methodJson.put("method", methodName);
                                    methodJson.put("complexity", complexity);
                                    methodsArray.add(methodJson);
                                    totalComplexity += complexity;
                                    totalMethods++;
                                    inMethod = false;
                                }
                            }
                        }

                        if (!methodsArray.isEmpty()) {
                            fileJson.put("methods", methodsArray);
                            files.add(fileJson);
                        }
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                });

            report.put("files", files);
            report.put("totalComplexity", totalComplexity);
            report.put("averageComplexity", totalMethods > 0 ? (double) totalComplexity / totalMethods : 0.0);

            try (FileWriter writer = new FileWriter(outputFile)) {
                writer.write(report.toJSONString());
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}