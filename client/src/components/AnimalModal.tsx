import { motion, AnimatePresence } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { X, Star, User, Plus } from "lucide-react";
import type { Animal } from "./AnimalCard";

interface AnimalModalProps {
  animal: Animal | null;
  isOpen: boolean;
  onClose: () => void;
  onAddToCollection?: (animalId: string) => void;
  onAuthRequired?: () => void;
  isLoggedIn?: boolean;
}

export default function AnimalModal({ 
  animal, 
  isOpen, 
  onClose, 
  onAddToCollection,
  onAuthRequired,
  isLoggedIn = false 
}: AnimalModalProps) {
  if (!animal) return null;

  const handleAddToCollection = () => {
    if (isLoggedIn) {
      onAddToCollection?.(animal.id);
      console.log('Added to collection:', animal.name);
    } else {
      onAuthRequired?.();
      console.log('Authentication required');
    }
  };

  const rarityColors = {
    common: 'bg-secondary',
    rare: 'bg-accent',  
    legendary: 'bg-primary'
  };

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 bg-background/80 backdrop-blur-sm z-50 flex items-center justify-center p-4"
          onClick={onClose}
        >
          <motion.div
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.8, opacity: 0 }}
            transition={{ type: "spring", stiffness: 300, damping: 30 }}
            onClick={(e) => e.stopPropagation()}
            className="w-full max-w-md max-h-[90vh] overflow-auto"
          >
            <Card className="relative">
              <Button
                variant="ghost"
                size="icon"
                className="absolute top-2 right-2 z-10"
                onClick={onClose}
                data-testid="button-close-modal"
              >
                <X className="w-4 h-4" />
              </Button>
              
              <CardHeader className="pb-0">
                <div className="relative aspect-square rounded-lg overflow-hidden mb-4">
                  <motion.img
                    src={animal.imageUrl}
                    alt={animal.name}
                    className="w-full h-full object-cover bg-gradient-to-br from-background to-muted"
                    animate={{ 
                      y: [0, -5, 0],
                      scale: [1, 1.02, 1]
                    }}
                    transition={{ 
                      duration: 3, 
                      repeat: Infinity, 
                      ease: "easeInOut" 
                    }}
                    data-testid="img-modal-animal"
                  />
                  <div className="absolute top-2 left-2">
                    <Badge className={`${rarityColors[animal.rarity]} text-xs`}>
                      <Star className="w-3 h-3 mr-1" />
                      {animal.rarity}
                    </Badge>
                  </div>
                </div>
                
                <CardTitle className="font-playful text-2xl text-center" data-testid="text-modal-name">
                  {animal.name}
                </CardTitle>
                <p className="text-muted-foreground text-center" data-testid="text-modal-species">
                  {animal.species}
                </p>
              </CardHeader>
              
              <CardContent className="space-y-4">
                <div>
                  <h4 className="font-semibold mb-2">Habitat</h4>
                  <p className="text-sm text-muted-foreground" data-testid="text-modal-habitat">
                    {animal.habitat}
                  </p>
                </div>
                
                <div>
                  <h4 className="font-semibold mb-2">Fun Facts</h4>
                  <ul className="space-y-2">
                    {animal.facts.map((fact, index) => (
                      <li key={index} className="text-sm text-muted-foreground flex items-start gap-2">
                        <div className="w-1.5 h-1.5 rounded-full bg-primary flex-shrink-0 mt-2"></div>
                        <span data-testid={`text-fact-${index}`}>{fact}</span>
                      </li>
                    ))}
                  </ul>
                </div>
                
                <Button 
                  className="w-full bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90"
                  onClick={handleAddToCollection}
                  data-testid="button-add-collection"
                >
                  {isLoggedIn ? (
                    <>
                      <Plus className="w-4 h-4 mr-2" />
                      Add to Collection
                    </>
                  ) : (
                    <>
                      <User className="w-4 h-4 mr-2" />
                      Login to Collect
                    </>
                  )}
                </Button>
              </CardContent>
            </Card>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}