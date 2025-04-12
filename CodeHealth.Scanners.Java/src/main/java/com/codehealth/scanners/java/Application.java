package com.codehealth.scanners.java;

public class Application {
    public static void main(String[] args) {
        System.out.println("Java scanners running!");
        
        CyclomaticComplexityScanner.run();
        TodoScanner.run();
    }
}