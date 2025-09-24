import { useState, useEffect } from "react";
import { Switch, Route } from "wouter";
import { queryClient } from "./lib/queryClient";
import { QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import BottomNav from "@/components/BottomNav";
import ScanPage from "@/pages/ScanPage";
import CollectionPage from "@/pages/CollectionPage";
import ProfilePage from "@/pages/ProfilePage";

function Router() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  // Parse URL parameters for NFC functionality
  useEffect(() => {
    const urlParams = new URLSearchParams(window.location.search);
    const animalId = urlParams.get('id');
    
    if (animalId) {
      console.log('NFC tag scanned with animal ID:', animalId);
      // todo: replace with real NFC handling logic
      // This would trigger the scan flow for the specific animal
    }
  }, []);

  const handleLogin = () => {
    setIsLoggedIn(true);
    console.log('User logged in');
    // todo: replace with real authentication logic
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    console.log('User logged out');
    // todo: replace with real logout logic
  };

  return (
    <Switch>
      <Route path="/">
        <ScanPage 
          isLoggedIn={isLoggedIn} 
          onLogin={handleLogin}
        />
      </Route>
      <Route path="/collection">
        <CollectionPage 
          isLoggedIn={isLoggedIn} 
          onLogin={handleLogin}
        />
      </Route>
      <Route path="/profile">
        <ProfilePage 
          isLoggedIn={isLoggedIn} 
          onLogin={handleLogin}
          onLogout={handleLogout}
        />
      </Route>
      {/* Fallback for any unmatched routes */}
      <Route>
        <ScanPage 
          isLoggedIn={isLoggedIn} 
          onLogin={handleLogin}
        />
      </Route>
    </Switch>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <div className="relative min-h-screen bg-background">
          <Router />
          <BottomNav />
          <Toaster />
        </div>
      </TooltipProvider>
    </QueryClientProvider>
  );
}

export default App;