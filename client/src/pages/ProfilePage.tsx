import { useState } from "react";
import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import AuthModal from "@/components/AuthModal";
import { User, Trophy, Star, Zap, LogOut, Settings } from "lucide-react";

interface ProfilePageProps {
  isLoggedIn?: boolean;
  onLogin?: () => void;
  onLogout?: () => void;
}

export default function ProfilePage({ isLoggedIn = false, onLogin, onLogout }: ProfilePageProps) {
  const [showAuthModal, setShowAuthModal] = useState(false);

  const handleAuthSuccess = () => {
    setShowAuthModal(false);
    onLogin?.();
  };

  // todo: remove mock functionality - replace with real user data from backend
  const mockUserStats = {
    name: "Animal Explorer",
    joinDate: "December 2024",
    totalAnimals: 6,
    collectedAnimals: 3,
    rareAnimals: 2,
    legendaryAnimals: 1,
    achievements: [
      { id: 1, name: "First Discovery", description: "Found your first animal", earned: true },
      { id: 2, name: "Collector", description: "Collected 5 animals", earned: false },
      { id: 3, name: "Legendary Hunter", description: "Found a legendary animal", earned: true },
      { id: 4, name: "Explorer", description: "Discovered 10 different species", earned: false },
    ]
  };

  if (!isLoggedIn) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-muted/20 to-accent/10 flex flex-col items-center justify-center p-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center space-y-6"
        >
          <div className="text-8xl">👤</div>
          <div className="space-y-2">
            <h2 className="font-playful text-2xl text-foreground">
              Join the Adventure!
            </h2>
            <p className="text-muted-foreground max-w-xs">
              Create an account to track your progress and unlock achievements
            </p>
          </div>
          
          <Button 
            onClick={() => setShowAuthModal(true)}
            className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90"
            data-testid="button-create-profile"
          >
            <User className="w-4 h-4 mr-2" />
            Create Account
          </Button>
        </motion.div>

        <AuthModal
          isOpen={showAuthModal}
          onClose={() => setShowAuthModal(false)}
          onLogin={handleAuthSuccess}
          onSignup={handleAuthSuccess}
        />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-muted/20 to-accent/10 pb-20">
      <div className="p-6 space-y-6">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center space-y-4"
        >
          <Avatar className="w-20 h-20 mx-auto" data-testid="avatar-profile">
            <AvatarImage src="/placeholder-avatar.jpg" />
            <AvatarFallback className="text-2xl font-playful bg-primary text-primary-foreground">
              AE
            </AvatarFallback>
          </Avatar>
          
          <div>
            <h1 className="font-playful text-2xl text-foreground" data-testid="text-username">
              {mockUserStats.name}
            </h1>
            <p className="text-sm text-muted-foreground" data-testid="text-join-date">
              Animal collector since {mockUserStats.joinDate}
            </p>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.2 }}
          className="grid grid-cols-2 gap-4"
        >
          <Card className="text-center">
            <CardContent className="p-4">
              <div className="text-2xl font-bold text-primary" data-testid="text-collected-count">
                {mockUserStats.collectedAnimals}
              </div>
              <div className="text-xs text-muted-foreground">Collected</div>
            </CardContent>
          </Card>
          
          <Card className="text-center">
            <CardContent className="p-4">
              <div className="text-2xl font-bold text-accent" data-testid="text-completion-rate">
                {Math.round((mockUserStats.collectedAnimals / mockUserStats.totalAnimals) * 100)}%
              </div>
              <div className="text-xs text-muted-foreground">Complete</div>
            </CardContent>
          </Card>
          
          <Card className="text-center">
            <CardContent className="p-4">
              <div className="text-2xl font-bold text-chart-2" data-testid="text-rare-count">
                {mockUserStats.rareAnimals}
              </div>
              <div className="text-xs text-muted-foreground">Rare</div>
            </CardContent>
          </Card>
          
          <Card className="text-center">
            <CardContent className="p-4">
              <div className="text-2xl font-bold text-chart-3" data-testid="text-legendary-count">
                {mockUserStats.legendaryAnimals}
              </div>
              <div className="text-xs text-muted-foreground">Legendary</div>
            </CardContent>
          </Card>
        </motion.div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.4 }}
        >
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-lg">
                <Trophy className="w-5 h-5 text-primary" />
                Achievements
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {mockUserStats.achievements.map((achievement) => (
                <div
                  key={achievement.id}
                  className={`flex items-center gap-3 p-3 rounded-lg border ${
                    achievement.earned 
                      ? 'bg-primary/5 border-primary/20' 
                      : 'bg-muted/20 border-muted/40'
                  }`}
                  data-testid={`achievement-${achievement.id}`}
                >
                  <div className={`p-2 rounded-full ${
                    achievement.earned 
                      ? 'bg-primary text-primary-foreground' 
                      : 'bg-muted text-muted-foreground'
                  }`}>
                    <Star className="w-4 h-4" />
                  </div>
                  <div className="flex-1">
                    <h4 className={`font-semibold text-sm ${
                      achievement.earned ? 'text-foreground' : 'text-muted-foreground'
                    }`}>
                      {achievement.name}
                    </h4>
                    <p className="text-xs text-muted-foreground">
                      {achievement.description}
                    </p>
                  </div>
                  {achievement.earned && (
                    <Badge variant="secondary" className="text-xs">
                      Earned
                    </Badge>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
        </motion.div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          className="space-y-3"
        >
          <Button 
            variant="outline" 
            className="w-full justify-start"
            onClick={() => console.log('Settings clicked')}
            data-testid="button-settings"
          >
            <Settings className="w-4 h-4 mr-2" />
            Settings
          </Button>
          
          <Button 
            variant="outline" 
            className="w-full justify-start text-destructive hover:text-destructive"
            onClick={() => {
              console.log('Logout clicked');
              onLogout?.();
            }}
            data-testid="button-logout"
          >
            <LogOut className="w-4 h-4 mr-2" />
            Logout
          </Button>
        </motion.div>
      </div>
    </div>
  );
}